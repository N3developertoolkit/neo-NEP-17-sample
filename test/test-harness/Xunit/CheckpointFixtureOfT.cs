using System;

namespace NeoTestHarness.Xunit
{
    public class CheckpointFixture<T> : CheckpointFixture
    {
        static string GetCheckpointPath()
        {
            var attrib = ContractAttribute.GetCustomAttribute(typeof(T), typeof(CheckpointPathAttribute));
            if (attrib is CheckpointPathAttribute contractAttrib)
            {
                return contractAttrib.Path;
            }

            throw new Exception($"Missing {nameof(CheckpointPathAttribute)} on {typeof(T).Name}");
        }

        public CheckpointFixture() : base(GetCheckpointPath())
        {
        }
    }
}

