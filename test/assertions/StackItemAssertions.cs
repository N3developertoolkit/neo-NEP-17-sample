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

        //         public static void AssertEqual(this object? expected, StackItem actual)
        // {
        //     if (expected == null)
        //     {
        //         actual.IsNull.Should().BeTrue();
        //     }
        //     else
        //     {
        //         switch (expected)
        //         {
        //             case UInt160 expectedHash160:
        //                 Assert.Equal(expectedHash160, new UInt160(actual.GetSpan()));
        //                 break;
        //             case UInt256 expectedHash256:
        //                 Assert.Equal(expectedHash256, new UInt256(actual.GetSpan()));
        //                 break;
        //             case BigInteger expectedInt:
        //                 Assert.Equal(expectedInt, actual.GetInteger());
        //                 break;
        //             case bool expectedBool:
        //                 Assert.Equal(expectedBool, actual.GetBoolean());
        //                 break;
        //             case string expectedStr:
        //                 Assert.Equal(expectedStr, actual.GetString());
        //                 break;
        //             case byte[] expectedBytes:
        //                 Assert.True(expectedBytes.AsSpan().SequenceEqual(actual.GetSpan()));
        //                 break;
        //             case object[] _:
        //                 Assert.Equal(StackItemType.Array, actual.Type);
        //                 break;
        //             case ECPoint expectedECPoint:
        //                 Assert.Equal(expectedECPoint, ECPoint.DecodePoint(actual.GetSpan(), ECCurve.Secp256r1));
        //                 break;
        //             default:
        //                 Assert.False(true);
        //                 break;
        //         }
        //     }
        // }

        // public AndConstraint<StackItemAssertions> Be(UInt160 expected, string because = "", params object[] becauseArgs)
        // {
        //     Subject.Type.Should().Be(StackItemType.ByteString);
        //     Subject.GetSpan().Length.Should().Be(20);
        //     var subject = new UInt160(Subject.GetSpan());
        //     subject.Should().Be(expected);

            
        //     // Action act = () => Subject.GetSpan();
        //     // act.Should().NotThrow();

        //     // Subject.Type.Should().Match(t => t == 
            
        //     // Be(StackItemType.ByteString).
        //     // var q = Execute.Assertion
        //     //     .Given(() => new UInt160())
        //     //     .ForCondition(_ => false)
        //     //     .FailWith("foo");


        //     // Execute.Assertion
        //     //     .BecauseOf(because, becauseArgs)
        //     //     .ForCondition(Subject.Type == StackItemType.ByteString 
        //     //         || Subject.Type == StackItemType.Buffer)
        //     //     .FailWith("Invalid StackItem type {0}", Subject.Type)
        //     //     .Then
        //     //     .ForCondition(Subject.GetSpan().Length == 20)
        //     //     .FailWith("Invalid StackItem span length {0}", Subject.GetSpan().Length)
        //     //     .Then
        //     //     .Given<UInt160>(() => new UInt160(Subject.GetSpan())
        //     //     .ForCondition(uint160 => expected.Equals(uint160)))
        //     //     .FailWith("Expected {context:value} to be {0}{reason}, but found {1}", 
        //     //         _ => expected, uint160 => uint160);

        //     return new AndConstraint<StackItemAssertions>(this);
        // }

        // public AndConstraint<StackItemAssertions> Be(BigInteger expected, string because = "", params object[] becauseArgs)
        // {
        //     Execute.Assertion
        //         .BecauseOf(because, becauseArgs)
        //         .ForCondition(Subject.Type == StackItemType.Integer)
        //         .FailWith("Invalid StackItem type {0}", Subject.Type)
        //         .Then
        //         .ForCondition(expected == Subject.GetInteger())
        //         .FailWith("Expected {context:value} to be {0}{reason}, but found {1}", expected, Subject.GetInteger());

        //     return new AndConstraint<StackItemAssertions>(this);
        // }

        // public AndConstraint<StackItemAssertions> BeNull(string because = "", params object[] becauseArgs)
        // {
        //     Execute.Assertion
        //         .BecauseOf(because, becauseArgs)
        //         .ForCondition(Subject.IsNull)
        //         .FailWith("Expected {context:value} to be null");

        //     return new AndConstraint<StackItemAssertions>(this);
        // }

        // public AndConstraint<StackItemAssertions> Be(object? expected, string because = "", params object[] becauseArgs)
        // {
        //     return expected switch
        //     {
        //         null => BeNull(because, becauseArgs),
        //         UInt160 uint160 => Be(uint160, because, becauseArgs),
        //         BigInteger bigInteger => Be(bigInteger, because, becauseArgs),
        //         _ => InvalidType()
        //     };

        //     AndConstraint<StackItemAssertions> InvalidType()
        //     {
        //         Execute.Assertion
        //             .BecauseOf(because, becauseArgs)
        //             .ForCondition(false)
        //             .FailWith("Invalid expected type {0}", expected.GetType().Name);
        //         return new AndConstraint<StackItemAssertions>(this);
        //     }
        // }
    }
}
