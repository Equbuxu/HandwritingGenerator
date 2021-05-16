using System.Collections.ObjectModel;

namespace Handwriting_Generator.UI
{
    class BindCharacter
    {
        public FChar Id { get; }
        public ObservableCollection<BindSample> Samples { get; }

        public BindCharacter(FChar id)
        {
            Id = id;
            Samples = new ObservableCollection<BindSample>();
        }
    }
}
