using System;
using System.Threading.Tasks;
using Accardi_Alessandro_Refuge.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge.CoucheMetier;
using Accardi_Alessandro_Refuge.CouchePresentation;

internal class Program
{
    static async Task Main(string[] args)
    {
        await Presentation.Instance.MenuPrincipal();

    }
}
