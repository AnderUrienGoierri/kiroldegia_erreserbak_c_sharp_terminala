namespace kiroldegia_erreserbak_c_sharp_terminala.Models;

public class Erreserba
{
    public int Id { get; set; }
    public string Lekua { get; set; } = "";
    public DateOnly Data { get; set; }
    public TimeOnly Ordua { get; set; }
    public string Emaila { get; set; } = "";
    public bool Ezabatua { get; set; }
    public DateTime? EzabaketaData { get; set; }
}
