using AutoMapper;
using ApiEmpresas.Models;
using ApiEmpresas.DTOs;

namespace ApiEmpresas
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Empresa, EmpresaDto>().ReverseMap();
            CreateMap<Funcionario, FuncionarioDto>().ReverseMap();
        }
    }
}
