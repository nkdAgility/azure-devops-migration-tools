namespace MigrationTools._Enginev1.Containers
{
    public class WitMapper : IWitdMapper
    {
        private string _MapTo;

        public WitMapper(string mapTo)
        {
            _MapTo = mapTo;
        }

        public string Map()
        {
            return _MapTo;
        }
    }
}