using kiroldegia_erreserbak_c_sharp_terminala.Services;

namespace kiroldegia_erreserbak_c_sharp_terminala.GUI;

public static class GuiLauncher
{
    /// <summary>
    /// Windows Forms interfaze grafikoa abiarazten du STA hari dedikatu batean.
    /// Modu honetan kontsolatik zein komando lerrotik seguru exekutatu daiteke.
    /// </summary>
    public static void AbiaraziGui(KiroldegiaApiClient api)
    {
        var thread = new Thread(() =>
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(api));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore larria interfaze grafikoa abiaraztean:\n{ex.Message}",
                    "Errorea",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
