using Accardi_Alessandro_Refuge.CouchePresentation;

internal class Program
{
    static async Task Main(string[] args)
    {
        await Presentation.Instance.MenuPrincipal();
    }
}
