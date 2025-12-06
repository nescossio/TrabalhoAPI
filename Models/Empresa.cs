using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiEmpresas.Models
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; }

        [Required]
        [StringLength(100)]
        public string CNPJ { get; set; }

        public List<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();
    }
}
