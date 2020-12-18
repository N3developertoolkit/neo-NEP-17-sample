using System;
using System.Numerics;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Neo;
using Neo.VM.Types;


namespace NeoAssertions
{
    public class StackItemAssertions : ReferenceTypeAssertions<StackItem, StackItemAssertions>
    {
        public StackItemAssertions(StackItem subject)
        {
            Subject = subject;
        }

        protected override string Identifier => nameof(StackItem);

        public AndConstraint<StackItemAssertions> BeEquivalentTo(string expected, string because = "", params object[] becauseArgs)
        {
            try
            {
                var subject = Subject.GetString();

                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .ForCondition(subject == expected)
                    .FailWith("Expected {context:StackItem} to be of {0}{reason}, but found {1}.", expected, subject);
            }
            catch (Exception ex)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:StackItem} to support GetString{reason}, but GetString failed with:{0}.", ex.Message);
            }

            return new AndConstraint<StackItemAssertions>(this);
        }

        public AndConstraint<StackItemAssertions> BeEquivalentTo(BigInteger expected, string because = "", params object[] becauseArgs)
        {
            try
            {
                var subject = Subject.GetInteger();

                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .ForCondition(subject == expected)
                    .FailWith("Expected {context:StackItem} to be of {0}{reason}, but found {1}.", expected, subject);
            }
            catch (Exception ex)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:StackItem} to support GetInteger{reason}, but GetInteger failed with:{0}.", ex.Message);
            }

            return new AndConstraint<StackItemAssertions>(this);
        }

        public AndConstraint<StackItemAssertions> BeEquivalentTo(bool expected, string because = "", params object[] becauseArgs)
        {
            try
            {
                var subject = Subject.GetBoolean();

                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .ForCondition(subject == expected)
                    .FailWith("Expected {context:StackItem} to be of {0}{reason}, but found {1}.", expected, subject);
            }
            catch (Exception ex)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:StackItem} to support GetBoolean{reason}, but GetBoolean failed with:{0}.", ex.Message);
            }

            return new AndConstraint<StackItemAssertions>(this);
        }

        public AndConstraint<StackItemAssertions> BeTrue(string because = "", params object[] becauseArgs)
            => BeEquivalentTo(true, because, becauseArgs);

        public AndConstraint<StackItemAssertions> BeFalse(string because = "", params object[] becauseArgs)
            => BeEquivalentTo(false, because, becauseArgs);
    }
}
