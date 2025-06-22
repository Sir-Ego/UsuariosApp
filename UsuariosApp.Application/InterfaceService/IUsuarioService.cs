using UsuariosApp.Application.Dtos;

namespace UsuariosApp.Application.InterfaceService
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDto> CreateUsuarioAsync(UsuarioRequestDto dto);

        Task<UsuarioResponseDto> UpdateUsuarioAsync(UsuarioRequestDto dto, Guid id);

        Task DeleteUsuarioAsync(Guid id);

        Task<UsuarioResponseDto> GetUsuarioByIdAsync(Guid id);

        Task<IEnumerable<UsuarioResponseDto>> GetAllUsuariosAsync();
    }
}
