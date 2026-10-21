namespace kiroldegia_erreserbak_c_sharp_terminala.Models;

public class Pista
{
    public int Id { get; set; }
    public string Izena { get; set; } = "";
    public string Kirola { get; set; } = "";
    public bool Ezabatua { get; set; }
    public DateTime? EzabaketaData { get; set; }
}
