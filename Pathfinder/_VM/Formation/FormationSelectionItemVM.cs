using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.UI.MVVM._VM.Formation
{
	public class FormationSelectionItemVM : SelectionGroupEntityVM
	{
		public readonly int FormationIndex;

		public FormationSelectionItemVM(int formationIndex) : base(false)
		{
			FormationIndex = formationIndex;
		}

		protected override void DoSelectMe()
		{
		}
	}
}