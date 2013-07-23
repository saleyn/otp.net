using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ErlObject = Otp.Erlang.Object;
using ErlVarBind = Otp.Erlang.VarBind;

namespace Otp.Erlang
{
    public delegate void PatternMatchAction(
        PatternMatcher.Pattern pattern, ErlObject term,
        ErlVarBind binding, params object[] args);

    public delegate void PatternMatchAction<TContext>(
        TContext ctx, PatternMatcher.Pattern pattern, ErlObject term,
        ErlVarBind binding, params object[] args);

    public delegate void PatternMatchAction<TContext, TErlTerm>(
        TContext ctx, PatternMatcher.Pattern pattern, TErlTerm term,
        ErlVarBind binding, params object[] args) where TErlTerm : ErlObject;

    /// <summary>
    /// Pattern matcher that implements a container of patterns to be
    /// matched against a given Erlang term.  On successful match, the
    /// corresponding action registered with that pattern gets invoked
    /// </summary>
    public class PatternMatcher : IEnumerable<PatternMatcher.Pattern>
    {
        #region Local Classes

            public struct Pattern
            {
                public readonly int                 ID;
                public readonly PatternMatchAction  Action;
                public readonly Erlang.Object       Term;

                public Pattern(int id, PatternMatchAction b, Erlang.Object p)
                {
                    ID = id; Action = b; Term = p;
                }

                public Pattern(int id, PatternMatchAction b, string pattern, params object[] args)
                {
                    ID = id; Action = b; Term = Object.Format(pattern, args);
                }
            }

        #endregion

        #region Fields

            List<Pattern>  m_patterns = new List<Pattern>();
            int            m_lastID   = 0;

        #endregion

        #region Public

            /// <summary>
            /// Add a matching pattern to the collection
            /// </summary>
            /// <param name="action">Action to invoke on successful match</param>
            /// <param name="pattern">Pattern to compile</param>
            /// <param name="args">Arguments used in the pattern</param>
            /// <returns>ID of the newly added pattern</returns>
            public int Add(PatternMatchAction action, string pattern, params object[] args)
            {
                return Add(action, ErlObject.Format(pattern, args));
            }

            /// <summary>
            /// Add a matching pattern to the collection
            /// </summary>
            /// <param name="action">Action to invoke on successful match</param>
            /// <param name="pattern">Pattern to compile</param>
            /// <param name="args">Arguments used in the pattern</param>
            /// <returns>ID of the newly added pattern</returns>
            //public int Add(PatternMatchAction action, )
            //{
            //    int id = ++m_lastID;
            //    var pt = new Pattern(id, (p, t, b, args) => action(context, p, t.Cast<TErlTerm>(), b, args), pattern);
            //    m_patterns.Add(pt);
            //    return id;
            //}

            /// <summary>
            /// Add a matching pattern to the collection
            /// </summary>
            /// <param name="action">Action to invoke on successful match</param>
            /// <param name="pattern">Pattern to compile</param>
            /// <param name="args">Arguments used in the pattern</param>
            /// <returns>ID of the newly added pattern</returns>
            public int Add<TErlTerm>(PatternMatchAction action, TErlTerm pattern) where TErlTerm : ErlObject
            {
                int id = ++m_lastID;
                var pt = new Pattern(id, (p, t, b, args) => action(p, t.Cast<TErlTerm>(), b, args), pattern);
                m_patterns.Add(pt);
                return id;
            }

            /// <summary>
            /// Add a matching pattern to the collection
            /// </summary>
            /// <typeparam name="TContext">Type of context passed to action</typeparam>
            /// <param name="context">Context passed to action</param>
            /// <param name="action">Action to invoke on successful match</param>
            /// <param name="pattern">Pattern to compile</param>
            /// <param name="args">Arguments used in the pattern</param>
            /// <returns>ID of the newly added pattern</returns>
            public int Add<TContext>(TContext context, PatternMatchAction<TContext> action, string pattern, params object[] args)
            {
                return Add(context, action, ErlObject.Format(pattern, args));
            }

            /// <summary>
            /// Add a matching pattern to the collection
            /// </summary>
            /// <typeparam name="TContext">Type of context passed to action</typeparam>
            /// <param name="context">Context passed to action</param>
            /// <param name="action">Action to invoke on successful match</param>
            /// <param name="pattern">Pattern to compile</param>
            /// <param name="args">Arguments used in the pattern</param>
            /// <returns>ID of the newly added pattern</returns>
            public int Add<TContext, TErlTerm>(TContext context,
                PatternMatchAction<TContext, TErlTerm> action, string pattern, params object[] args
            ) where TErlTerm : ErlObject
            {
                return Add(context, action, ErlObject.Format(pattern, args).Cast<TErlTerm>());
            }

            /// <summary>
            /// Add a matching pattern to the collection
            /// </summary>
            /// <typeparam name="TContext">Type of context passed to action</typeparam>
            /// <param name="context">Context passed to action</param>
            /// <param name="action">Action to invoke on successful match</param>
            /// <param name="pattern">Compiled pattern containing variables to match</param>
            /// <returns>ID of the newly added pattern</returns>
            public int Add<TContext>(TContext context, PatternMatchAction<TContext> action, ErlObject pattern)
            {
                int id = ++m_lastID;
                var pt = new Pattern(id, (p, t, b, args) => action(context, p, t, b, args), pattern);
                m_patterns.Add(pt);
                return id;
            }

            /// <summary>
            /// Add a matching pattern to the collection
            /// </summary>
            /// <typeparam name="TContext">Type of context passed to action</typeparam>
            /// <param name="context">Context passed to action</param>
            /// <param name="action">Action to invoke on successful match</param>
            /// <param name="pattern">Compiled pattern containing variables to match</param>
            /// <returns>ID of the newly added pattern</returns>
            public int Add<TContext, TErlTerm>(
                TContext context,
                PatternMatchAction<TContext, TErlTerm> action,
                TErlTerm pattern) where TErlTerm : ErlObject
            {
                int id = ++m_lastID;
                var pt = new Pattern(id, (p, t, b, args) => action(context, p, t.Cast<TErlTerm>(), b, args), pattern);
                m_patterns.Add(pt);
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
            public int Match<TErlTerm>(TErlTerm term, params object[] args) where TErlTerm : ErlObject
            {
                var binding = new VarBind();

                foreach (var p in m_patterns)
                {
                    if (p.Term.match(term, binding))
                    {
                        p.Action(p, term, binding, args);
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

            public ErlObject PatternsToTerm
            {
                get { return new Erlang.List(m_patterns.Select(p => p.Term).ToArray()); }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_patterns.GetEnumerator();
            }

            IEnumerator<Pattern> IEnumerable<PatternMatcher.Pattern>.GetEnumerator()
            {
                return m_patterns.GetEnumerator();
            }

        #endregion
    }
}
