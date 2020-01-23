using System;
using System.Text;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * fixme
     * Consider whether we should just fold KC_EvaluatablePrologExpression functionality into KC_PrologExpression. Need to recapture the design rational for
     * splitting it out in this way. 
     */
    public class KC_EvaluatablePrologExpression : KC_PrologExpression
    {
        /*
         * True if this expression has been evaluated, false otherwise. Need this flag so you can tell the difference between an EvalResult of false with no bindings 
         * and an expression that just hasn't been evaluated yet.         
         */
        public bool Evaluated { get; protected set; }

        /*
         * The boolean result of evaluating the prolog expresssion (true if prolog can prove the expression, false otherwise). 
         */
        public bool EvalResult { get; protected set; }

        /*
         * Any bindings resulting from evaluating the prolog expression. 
         * fixme: currently storing UnityProlog style eval result (a Symbol or a LogicVariable). Eventually come up with an implementation-independent 
         * represention of bindings so that KC_PrologExpression doesn't care which prolog is used. 
         */
        public object EvalBindings { get; protected set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendFormat("Name: {0}, PrologExp: {1}, Evaluated: {2}, EvalResult: {3}, EvalBindings: {4}",
                PrologExpName, PrologExp, Evaluated, EvalResult, EvalBindings);
            if (ReadOnly)
            {
                sb.Append(", readonly)");
            }
            else
            {
                sb.Append(")");
            }
            return sb.ToString();
        }

        public override object Clone() => new KC_EvaluatablePrologExpression(this);

        /*
         * Evaluate the prolog expression against the KB. Once this expression has been evaluated, it can't be reevaluated or reset. 
         */
        public void Evaluate(KC_PrologKB kb)
        {
            if (!Evaluated)
            {
                var result = kb.SolveForParsed(PrologExp);
                EvalResult = result != null;
                EvalBindings = result;
                Evaluated = true;

                // Make readonly if it is not already readonly. Would be confusing to have a fixed evaluation result but a changeable expression. 
                if (!ReadOnly)
                {
                    ReadOnly = true;
                }
            }
        }

        public KC_EvaluatablePrologExpression(string name) : base(name)
        {
            Evaluated = false;
            EvalResult = false;
            EvalBindings = null;
        }

        public KC_EvaluatablePrologExpression(string name, string prologExp) : base(name, prologExp)
        {
            Evaluated = false;
            EvalResult = false;
            EvalBindings = null;
        }

        public KC_EvaluatablePrologExpression(string name, string prologExp, bool readOnly) : base(name, prologExp, readOnly)
        {
            Evaluated = false;
            EvalResult = false;
            EvalBindings = null;
        }

        protected KC_EvaluatablePrologExpression(KC_EvaluatablePrologExpression toCopy) : base(toCopy)
        {
            Evaluated = toCopy.Evaluated;
            EvalResult = toCopy.EvalResult;
            EvalBindings = toCopy.EvalBindings;
        }

        /*
         * This constructor copies the PrologExp and PrologExpName from a KC_PrologExpression, initing the other fields to unevaluated. 
         */
        public KC_EvaluatablePrologExpression(KC_PrologExpression pe) : base(pe.PrologExpName, pe.PrologExp, pe.ReadOnly)
        {
            Evaluated = false;
            EvalResult = false;
            EvalBindings = null;
        }
    }

    public static class KC_EvaluatblePrologExpression_Extensions
    {
        public static bool GetPrologExpEvaluated(this Unit unit)
        {
            if (unit.HasComponent<KC_EvaluatablePrologExpression>())
            {
                return unit.GetComponent<KC_EvaluatablePrologExpression>().Evaluated;
            }
            throw new InvalidOperationException("GetPrologExpEvaluated called on unit that does not have a KC_EvaluatablePrologExpression component.");
        }

        public static bool GetPrologExpEvalResult(this Unit unit)
        {
            if (unit.HasComponent<KC_EvaluatablePrologExpression>())
            {
                return unit.GetComponent<KC_EvaluatablePrologExpression>().EvalResult;
            }
            throw new InvalidOperationException("GetPrologExpEvaResult called on unit that does not have a KC_EvaluatablePrologExpression component.");
        }

        public static object GetPrologExpBindings(this Unit unit)
        {
            if (unit.HasComponent<KC_EvaluatablePrologExpression>())
            {
                return unit.GetComponent<KC_EvaluatablePrologExpression>().EvalBindings;
            }
            throw new InvalidOperationException("GetPrologExpBindings called on unit that does not have a KC_EvaluatablePrologExpression component.");
        }

    }
}
