﻿namespace Story.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Utils;

    [Serializable]
    public class Story : ContextBoundObject<Story>, IStory
    {
        private readonly Stopwatch stopWatch;
        private readonly IStoryLog log;
        private readonly IStoryData data;

        public Story(string name, IRuleset<IStory, IStoryHandler> handlerProvider)
        {
            try
            {
                Ensure.ArgumentNotEmpty(name, "name");
                Ensure.ArgumentNotNull(handlerProvider, "handlerProvider");

                this.HandlerProvider = handlerProvider;
                this.stopWatch = new Stopwatch();
                this.log = new StoryLog(this);
                this.data = new StoryData(this);

                if (this.Parent == null)
                {
                    this.Name = name;
                }
                else
                {
                    this.Name = this.Parent.Name + "/" + name;
                }
            }
            catch
            {
                base.Detach();
                throw;
            }
        }

        public string Name
        {
            get;
            private set;
        }

        public IRuleset<IStory, IStoryHandler> HandlerProvider
        {
            get;
            private set;
        }

        public new IStory Parent
        {
            get { return (IStory)base.Parent; }
        }

        public new IEnumerable<IStory> Children
        {
            get { return (IEnumerable<IStory>)base.Children; }
        }

        public IStoryData Data
        {
            get { return this.data; }
        }

        public IStoryLog Log
        {
            get { return this.log; }
        }

        public TimeSpan Elapsed
        {
            get { return this.stopWatch.Elapsed; }
        }

        public DateTime StartDateTime
        {
            get;
            private set;
        }

        public Task Task
        {
            get;
            set;
        }

        public void Start()
        {
            this.StartDateTime = DateTime.UtcNow;
            this.stopWatch.Start();

            foreach (var handler in this.HandlerProvider.Fire(this))
            {
                handler.OnStart(this);
            }
        }

        public void Stop()
        {
            this.stopWatch.Stop();

            try
            {
                foreach (var handler in this.HandlerProvider.Fire(this))
                {
                    handler.OnStop(this);
                }
            }
            finally
            {
                base.Detach();
            }
        }
    }
}
