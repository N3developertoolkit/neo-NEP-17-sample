using Neo.VM.Types;


namespace NeoAssertions
{
    public static class NeoAssertionsExtensions
    {
        public static StackItemAssertions Should(this StackItem item)
        {
            return new StackItemAssertions(item);
        }
    }
}
