using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Keeps track of the placeholder work items TfsEmbededImagesTool creates in the target, so each one
    /// is destroyed when it fills up and every one left is destroyed at the end of the run.
    /// </summary>
    internal class EmbededImagesPlaceholderTracker
    {
        private readonly int _capacity;
        private readonly Action<int> _destroy;
        private readonly ILogger _log;
        private readonly List<int> _pending = new List<int>();
        private int _uses;

        public EmbededImagesPlaceholderTracker(int capacity, Action<int> destroy, ILogger log)
        {
            _capacity = capacity;
            _destroy = destroy;
            _log = log;
        }

        /// <summary>The placeholder in use, or null when a new one is needed.</summary>
        public int? Current { get; private set; }

        /// <summary>Placeholders created but not yet destroyed, including the current one.</summary>
        public IReadOnlyList<int> Pending => _pending;

        public bool IsAtCapacity => Current != null && _uses > _capacity;

        public void Created(int id)
        {
            Current = id;
            _uses = 0;
            _pending.Add(id);
        }

        public void Used()
        {
            _uses++;
        }

        /// <summary>
        /// Destroys the current placeholder so a new one is created. If the destroy fails the id stays
        /// pending and is tried again at the end of the run.
        /// </summary>
        public void Retire()
        {
            if (Current is int id)
            {
                Current = null;
                _uses = 0;
                if (TryDestroy(id))
                {
                    _pending.Remove(id);
                }
            }
        }

        /// <summary>
        /// Destroys every pending placeholder, continuing past failures. Never throws, because it runs in
        /// the processor's finally block. Returns the ids that could not be destroyed.
        /// </summary>
        public List<int> DestroyAll()
        {
            Current = null;
            _uses = 0;
            var remaining = new List<int>();
            foreach (var id in _pending)
            {
                if (!TryDestroy(id))
                {
                    remaining.Add(id);
                }
            }
            _pending.Clear();
            return remaining;
        }

        private bool TryDestroy(int id)
        {
            try
            {
                _destroy(id);
                _log.LogInformation("TfsEmbededImagesTool: Destroyed placeholder work item {Id} in the target.", id);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "TfsEmbededImagesTool: Unable to destroy placeholder work item {Id}.", id);
                return false;
            }
        }
    }
}
