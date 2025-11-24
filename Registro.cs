namespace MindMonitor
{
    public class Registro
    {
        public int Id { get; set; }
        public string Colaborador { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public int Desmotivacao { get; set; }
        public int Sobrecarga { get; set; }
        public int Estresse { get; set; }
    }
}
