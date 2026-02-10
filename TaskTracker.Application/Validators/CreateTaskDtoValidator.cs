using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.DTOs;
using TaskTracker.Infrastructure.Data;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator(AppDbContext context)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(3, 200);

        RuleFor(x => x.AssigneeId)
            .MustAsync(async (id, ct) =>
                await context.Users.AnyAsync(u => u.Id == id, ct))
            .WithMessage("Исполнитель не существует");

        RuleFor(x => x.DueDate)
            .Must(d => d == null || d > DateTime.UtcNow)
            .WithMessage("DueDate должен быть в будущем");
    }
}
