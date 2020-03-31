using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;


namespace Components.Locker
{
    public enum Priority
    {
        Critical = 0,
        High = 1,
        Normal = 2,
        Low = 3,
    }

    public class ObjectLocker
    {
        public abstract class Exclusive : MarshalByRefObject
        {

            /// <summary>
            /// 自動的に切断されるのを回避する
            /// </summary>
            public override object InitializeLifetimeService()
            {
                return null;
            }

            internal Process ParentProcess { get; set; }

            [Serializable()]
            public class Key : IDisposable
            {
                internal delegate void ReleaseMethod(Key key);
                private ReleaseMethod Release { get; set; }
                internal Priority Priority { get; private set; }
                internal string Hash { get; private set; }

                public bool HoldLock { get; private set; } = false;
                internal Key(Priority priority, string hash, ReleaseMethod release)
                {
                    Priority = priority;
                    Hash = hash;
                    Release = release;
                    HoldLock = true;
                }
                internal Key()
                {
                }

                void IDisposable.Dispose()
                {
                    if (HoldLock)
                    {
                        Release(this);
                    }
                }
            }

            [Serializable()]
            private class Event
            {
                public System.Threading.CountdownEvent Signal { get; set; }
                public Priority Priority { get; set; }
                public string Hash { get; set; }
            }

            #region stacklist
            private int TargetPriority { get; set; }
            private string TargetHash { get; set; } = string.Empty;
            private string TrushHash { get; set; } = string.Empty;
            private BlockingCollection<Event> Signal { get; set; } = new BlockingCollection<Event>();
            private List<Queue<Event>> EventList { get; set; } = new List<Queue<Event>>()
            {
                new Queue<Event>(),
                new Queue<Event>(),
                new Queue<Event>(),
                new Queue<Event>(),
            };
            #endregion

            internal void DistributionThread()
            {
                new Task(() =>
                {
                    while (true)
                    {
                        while (Signal.Count > 0)
                        {
                            Event e;
                            Signal.TryTake(out e);
                            EventList[(int)e.Priority].Enqueue(e);
                        }
                        if (TargetHash != string.Empty && TargetHash == TrushHash)
                        {
                            EventList[TargetPriority].Dequeue();
                            TargetHash = string.Empty;
                            TrushHash = string.Empty;
                        }
                        if (TargetHash == string.Empty)
                        {
                            for (int i = 0; i < EventList.Count; i++)
                            {
                                if (EventList[i].Count > 0)
                                {
                                    TargetPriority = i;
                                    var target = EventList[i].Peek();
                                    TargetHash = target.Hash;
                                    target.Signal.Signal();
                                    break;
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(1);
                    }
                }).Start();
            }

            #region Lock method
            public Key Lock(Priority priority = Priority.Normal)
            {
                try
                {
                    var hash = HashCreator.GetString(32);
                    var e = new Event()
                    {
                        Priority = priority,
                        Hash = hash,
                        Signal = new System.Threading.CountdownEvent(1)
                    };
                    Signal.TryAdd(e);
                    e.Signal.Wait();
                    return new Key(priority, hash, Release);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            public Key LockThrow()
            {
                try
                {
                    if (Signal.Count == 0)
                    {
                        var hash = HashCreator.GetString(32);
                        var e = new Event()
                        {
                            Priority = Priority.Critical,
                            Hash = hash,
                            Signal = new System.Threading.CountdownEvent(1)
                        };
                        Signal.TryAdd(e);
                        e.Signal.Wait();
                        return new Key(e.Priority, hash, Release);
                    }
                    return new Key();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public void Release(Key key)
            {
                TrushHash = key.Hash;
                while (TrushHash != string.Empty) { }
            }
            #endregion
        }

        public static Exclusive CreateServer(string channelname, string objectname, Type objecttype)
        {
            var channelProperties = new Dictionary<string, string>();
            channelProperties.Add("portName", channelname);
            // サーバーチャンネルの生成
            IpcChannel channel = new IpcChannel(channelProperties, null, new BinaryServerFormatterSinkProvider
            {
                TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full,
            });
            // チャンネルを登録
            ChannelServices.RegisterChannel(channel, true);

            // リモートオブジェクトを生成して公開
            var key = (Exclusive)Activator.CreateInstance(objecttype);
            RemotingServices.Marshal(key, objectname, objecttype);
            key.DistributionThread();
            return key;
        }

        public static Exclusive CreateClient(string channelname, string objectname)
        {
            // クライアントチャンネルの生成
            IpcClientChannel channel = new IpcClientChannel();
            // チャンネルを登録
            ChannelServices.RegisterChannel(channel, true);

            // リモートオブジェクトを取得
            var key = Activator.GetObject(typeof(Exclusive), "ipc://" + channelname + "/" + objectname) as Exclusive;
            return key;
        }
    }
}
