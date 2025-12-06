using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresas.Models
{
    public class Funcionario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Nome { get; set; }

        [Required]
        [StringLength(100)]
        public string Cargo { get; set; }

        [Range(0, 1000000, ErrorMessage = "O salário deve estar entre 0 e 1.000.000.")]
        public decimal Salario { get; set; }

        [ForeignKey("Empresa")]
        public int EmpresaId { get; set; }

        public Empresa? Empresa { get; set; }
    }
}
