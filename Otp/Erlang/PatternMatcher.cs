using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Otp.Erlang
{
    /// <summary>
    /// Pattern matcher that implements a container of patterns to be
    /// matched against a given Erlang term.  On successful match, the
    /// corresponding action registered with that pattern gets invoked
    /// </summary>
    public class PatternMatcher
    {
        struct KeyValueData
        {
            public readonly int ID;
            public readonly Erlang.Object Pattern;
            public readonly Action<int, Erlang.VarBind> Action;

            public KeyValueData(int id, Erlang.Object p, Action<int, Erlang.VarBind> b)
            {
                ID = id; Pattern = p; Action = b;
            }
        }

        List<KeyValueData>  m_patterns = new List<KeyValueData>();
        int                 m_lastID   = 0;

        /// <summary>
        /// Add a matching pattern to the collection
        /// </summary>
        /// <typeparam name="TContext">Type of context passed to matchAction</typeparam>
        /// <param name="context">Context passed to matchAction</param>
        /// <param name="matchAction">Action to invoke on successful match</param>
        /// <param name="pattern">Pattern to compile</param>
        /// <param name="args">Arguments used in the pattern</param>
        /// <returns>ID of the newly added pattern</returns>
        public int Add<TContext>(TContext context, Action<int, Erlang.VarBind, TContext> matchAction,
            string pattern, params object[] args)
        {
            int id = ++m_lastID;
            var p = new KeyValueData(id, Erlang.Object.Format(pattern, args), (i, b) => matchAction(i, b, context));
            m_patterns.Add(p);
            return id;
        }

        /// <summary>
        /// Remove pattern from collection given its ID
        /// </summary>
        public void Remove(int id)
        {
            int i = m_patterns.FindIndex(d => d.ID == id);

            if (i != -1)
                m_patterns.RemoveAt(i);
        }

        /// <summary>
        /// Match a term against the patterns in the collection.
        /// The first successful match will result in invokation of the action
        /// associated with the pattern
        /// </summary>
        /// <param name="term">Term to match against patterns</param>
        /// <returns>ID of the pattern that matched, or -1 if there were no matches</returns>
        public int Match(Erlang.Object term)
        {
            var binding = new VarBind();

            foreach (var p in m_patterns)
            {
                if (p.Pattern.match(term, binding))
                {
                    p.Action(p.ID, binding);
                    return p.ID;
                }
                binding.clear();
            }

            return -1;
        }

        /// <summary>
        /// Clear the collection of patterns
        /// </summary>
        public void Clear()
        {
            m_patterns.Clear();
        }
    }
}
