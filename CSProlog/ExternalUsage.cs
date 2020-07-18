/*-----------------------------------------------------------------------------------------

  C#Prolog -- Copyright (C) 2007-2015 John Pool -- j.pool@ision.nl

  This library is free software; you can redistribute it and/or modify it under the terms of
  the GNU Lesser General Public License as published by the Free Software Foundation; either 
  version 3.0 of the License, or any later version.

  This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
  See the GNU Lesser General Public License (http://www.gnu.org/licenses/lgpl-3.0.html), or 
  enter 'license' at the command prompt.

-------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Prolog
{
    #region SolutionSet
    public class SolutionSet
    {
        string query;
        public string Query { get { return query; } internal set { query = value; } }
        bool success;
        public bool Success { get { return success; } internal set { success = value; } }
        string errorMsg;
        public string ErrMsg { get { return errorMsg; } internal set { errorMsg = value; } }
        public bool HasError { get { return errorMsg != null; } }
        List<Solution> solutionSet;
        public int Count { get { return solutionSet.Count; } }
        Solution currVarSet;

        public SolutionSet()
        {
            solutionSet = new List<Solution>();
            success = false;
            errorMsg = null;
        }

        internal void CreateVarSet()
        {
            solutionSet.Add(currVarSet = new Solution());
        }

        internal void AddToVarSet(string name, string type, string value)
        {
            currVarSet.Add(name, type, value);
            success = true;
        }

        public IEnumerable<Solution> NextSolution
        {
            get
            {
                foreach (Solution s in solutionSet)
                    yield return s;
            }
        }

        public Solution this[int i]
        {
            get
            {
                return solutionSet[i];
            }
        }

        public override string ToString()
        {
            if (errorMsg != null)
                return errorMsg;

            if (success)
            {
                if (solutionSet.Count == 0)
                    return "yes";
                else
                {
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    foreach (Solution s in solutionSet)
                        sb.AppendLine("Solution {0}\r\n{1}", ++i, s.ToString());

                    return sb.ToString();
                }
            }
            else
                return "no";
        }
    }
    #endregion SolutionSet

    #region Solution
    public class Solution // a solution is a set of variables
    {
        List<Variable> variables;
        int Count { get { return variables.Count; } }

        public Solution()
        {
            variables = new List<Variable>();
        }

        internal void Add(string name, string type, string value)
        {
            variables.Add(new Variable(name, type, value));
        }

        public IEnumerable<Variable> NextVariable
        {
            get
            {
                foreach (Variable v in variables)
                    yield return v;
            }
        }

        public Variable this[int i]
        {
            get
            {
                return variables[i];
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Variable v in variables)
                sb.AppendLine(v.ToString());

            return sb.ToString();
        }
    }
    #endregion Solution

    #region Variable
    public class Variable
    {
        string name;
        public string Name { get { return name; } }
        string type;
        public string Type { get { return type; } }
        string value;
        public string Value { get { return value; } }

        public Variable(string name, string type, string value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}) = {2}", name, type, value);
        }
    }
    #endregion Variable

    public partial class PrologEngine
    {
        #region GetAllSolutions
        // Store solutions in an GetAllSolutions class
        public SolutionSet GetAllSolutions(string sourceFileName, string query)
        {
            return GetAllSolutions(sourceFileName, query, 0);
        }

        public SolutionSet GetAllSolutions(string sourceFileName, string query, int maxSolutionCount)
        {

            SolutionSet solutions = new SolutionSet();

            try
            {
                if (sourceFileName != null) Reset();

                if (sourceFileName != null) Consult(sourceFileName);

                Query = solutions.Query = query + (query.EndsWith(".") ? null : "."); // append a dot if necessary
                int i = 0;
                bool found = false;
                bool varFound = false;

                foreach (PrologEngine.ISolution s in SolutionIterator)
                {
                    if (Error)
                    {
                        solutions.ErrMsg = s.ToString();

                        break;
                    }
                    else if (!found && !s.Solved)
                        break;

                    solutions.Success = true;
                    bool firstVar = true;

                    foreach (PrologEngine.IVarValue varValue in s.VarValuesIterator)
                    {
                        if (varValue.DataType == "none") break;

                        if (firstVar)
                        {
                            firstVar = false;
                            solutions.CreateVarSet();
                        }

                        solutions.AddToVarSet(varValue.Name, varValue.DataType, varValue.Value.ToString());
                        varFound = true;
                    }

                    if (++i == maxSolutionCount || !varFound) break;

                    found = true;
                }
            }
            catch (Exception e)
            {
                solutions.ErrMsg = e.Message;
            }

            return solutions;
        }
        #endregion GetAllSolutions
    }
}
