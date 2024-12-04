using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools.Shadows
{
    public class MockStringManipulatorTool : IStringManipulatorTool
    {
        public string? ProcessString(string? value)
        {
            throw new NotImplementedException();
        }
    }
}
