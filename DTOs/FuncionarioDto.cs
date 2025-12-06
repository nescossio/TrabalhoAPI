namespace ApiEmpresas.DTOs
{
    public class FuncionarioDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cargo { get; set; }
        public decimal Salario { get; set; }
        public int EmpresaId { get; set; }
    }
}
