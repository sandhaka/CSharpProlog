using System;
using Xunit;
using Prolog;

namespace CSProlog.Core.Test
{
    public class PrologEngineTest
    {
        [Fact]
        public void ConsultFromString_GetOneSolution()
        {
            var prolog = new PrologEngine(persistentCommandHistory: false);

            // 'socrates' is human.
            prolog.ConsultFromString("human(socrates).");
            // 'R2-D2' is droid.
            prolog.ConsultFromString("droid(r2d2).");
            // human is bound to die.
            prolog.ConsultFromString("mortal(X) :- human(X).");

            // Question: Shall 'socrates' die?
            var solution1 = prolog.GetFirstSolution(query: "mortal(socrates).");
            Assert.True(solution1.Solved); // = "True" (Yes)

            // Question: Shall 'R2-D2' die?
            var solution2 = prolog.GetFirstSolution(query: "mortal(r2d2).");
            Assert.False(solution2.Solved); // = "False" (No)
        }

        [Fact]
        public void ConsultFromString_GetAllSolutions_Adhoc()
        {
            var prolog = new PrologEngine(persistentCommandHistory: false);

            // 'socrates' is human.
            prolog.ConsultFromString("human(socrates).");
            // 'R2-D2' is droid.
            prolog.ConsultFromString("droid(r2d2).");
            // human is bound to die.
            prolog.ConsultFromString("mortal(X) :- human(X).");

            prolog.GetFirstSolution(query: "listing.");

            SolutionSet solutionset1 = prolog.GetAllSolutions(null, "human(H)");
            Console.WriteLine("=====================================");
            Console.WriteLine(solutionset1.ErrMsg);
            Console.WriteLine("=====================================");
            Assert.True(solutionset1.Success);
            if (solutionset1.Success)
            {
                var s = solutionset1[0];
                foreach (Variable v in s.NextVariable)
                    Console.WriteLine(string.Format("{0} ({1}) = {2}", v.Name, v.Type, v.Value));

            }

        }

        [Fact]
        public void HelpTest()
        {
            var prolog = new PrologEngine(persistentCommandHistory: false);
            var s1 = prolog.GetFirstSolution("help.");
            Assert.True(s1.Solved); // = "True" (Yes) [help atleast ran]
        }

        /// <summary>
        /// A member() recursive implementation
        /// </summary>
        [Fact]
        public void UseMemImpl()
        {
            var prolog = new PrologEngine(persistentCommandHistory: false);

            prolog.ConsultFromString("mem(X,[X|List]). mem(X,[Y|List]) :- mem(X,List).");

            var sol1 = prolog.GetFirstSolution(query: "mem(a,[a,g,t,b]).");
            Assert.True(!prolog.Error & sol1.Solved);
            var sol2 = prolog.GetFirstSolution(query: "mem(z,[a,g,t,b]).");
            Assert.True(!prolog.Error);
            Assert.False(sol2.Solved);
            var sol3 = prolog.GetFirstSolution(query: "mem(g,[a,g,t,b]).");
            Assert.True(!prolog.Error & sol3.Solved);
            var sol4 = prolog.GetFirstSolution(query: "mem(a,[a]).");
            Assert.True(!prolog.Error & sol4.Solved);
            var sol5 = prolog.GetFirstSolution(query: "mem(a,[a].");
            Assert.True(prolog.Error & sol5.Solved);
        }
    }
}
