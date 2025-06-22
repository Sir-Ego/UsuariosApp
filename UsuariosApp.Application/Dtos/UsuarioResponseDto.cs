﻿namespace UsuariosApp.Application.Dtos
{
    public class UsuarioResponseDto
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Permissao { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }
}
