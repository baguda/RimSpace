using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimSpace
{
	public class CompSpacer : ThingComp
	{
		public CompSpacer() { }
		public CompProperties_Spacer Props => this.props as CompProperties_Spacer;

	}
}
