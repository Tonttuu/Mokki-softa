public class Varaus
{
    public int VarausId { get; set; }
    public int MokkiId { get; set; }
    public int AsiakasId { get; set; }
    public DateTime VarattuPvm { get; set; }
    public DateTime VahvistusPvm { get; set; }
    public DateTime VarattuAlkuPvm { get; set; }
    public DateTime VarattuLoppuPvm { get; set; }
}