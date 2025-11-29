using Microsoft.AspNetCore.Mvc;
using SmartQuiz.Services;
using SmartQuiz.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace SmartQuiz.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;

        public GroupsController(IGroupService groupService, IUserService userService)
        {
            _groupService = groupService;
            _userService = userService; // ✅ maintenant disponible
        }     

        // ✅ Détails d’un groupe
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var group = await _groupService.GetByIdAsync(id);
            if (group == null) return NotFound();

            // Tu peux soit passer directement le Group,
            // soit le mapper vers GroupInfoViewModel si tu préfères
            return View(group);
        }

        [HttpGet]
public async Task<IActionResult> Edit(string id, string? filterFiliere, int? filterAnnee)
{
    if (string.IsNullOrEmpty(id)) return NotFound();

    var group = await _groupService.GetByIdAsync(id);
    if (group == null) return NotFound();

    var allGroups = await _groupService.GetAllAsync();
    var allUsers = await _userService.GetAllUsers();
    var students = allUsers.Where(u => u.Role == "Student").ToList();

    if (!string.IsNullOrEmpty(filterFiliere) || filterAnnee.HasValue)
    {
        students = students.Where(s =>
        {
            if (string.IsNullOrEmpty(s.GroupId)) return true;
            var g = allGroups.FirstOrDefault(gr => gr.Id == s.GroupId);
            if (g == null) return false;
            bool ok = true;
            if (!string.IsNullOrEmpty(filterFiliere)) ok &= g.Filiere == filterFiliere;
            if (filterAnnee.HasValue) ok &= g.Annee == filterAnnee.Value;
            return ok;
        }).ToList();
    }

    var model = new EditGroupViewModel
    {
        Id = group.Id,
        Name = group.Name,
        Filiere = group.Filiere,
        Annee = group.Annee,
        Subject = group.Subject,
        IsActive = group.IsActive,
        Filieres = allGroups.Select(g => g.Filiere).Distinct().OrderBy(f => f).ToList(),
        Annees = allGroups.Select(g => g.Annee).Distinct().OrderBy(a => a).ToList(),
        FilterFiliere = filterFiliere,
        FilterAnnee = filterAnnee,
        Students = students.Select(s =>
        {
            var g = allGroups.FirstOrDefault(gr => gr.Id == s.GroupId);
            return new StudentInfoViewModel
            {
                Id = s.Id.ToString(),
                FullName = s.Username,
                Filiere = g?.Filiere ?? "-",
                Annee = g?.Annee ?? 0,
                GroupName = g?.Name ?? "Non attribué"
            };
        }).ToList(),
        SelectedStudentIds = group.StudentIds ?? new List<string>()
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditGroup(EditGroupViewModel model)
{
    if (!ModelState.IsValid)
    {
        // Recharge listes dropdown et étudiants
        var allGroups = await _groupService.GetAllAsync();
        model.Filieres = allGroups.Select(g => g.Filiere).Distinct().OrderBy(f => f).ToList();
        model.Annees = allGroups.Select(g => g.Annee).Distinct().OrderBy(a => a).ToList();

        var allUsers = await _userService.GetAllUsers();
        model.Students = allUsers.Where(u => u.Role == "Student").Select(s =>
        {
            var g = allGroups.FirstOrDefault(gr => gr.Id == s.GroupId);
            return new StudentInfoViewModel
            {
                Id = s.Id.ToString(),
                FullName = s.Username,
                Filiere = g?.Filiere ?? "-",
                Annee = g?.Annee ?? 0,
                GroupName = g?.Name ?? "Non attribué"
            };
        }).ToList();

        return View(model);
    }

    var groupToUpdate = await _groupService.GetByIdAsync(model.Id);
    if (groupToUpdate == null) return NotFound();

    groupToUpdate.Name = model.Name;
    groupToUpdate.Filiere = model.Filiere;
    groupToUpdate.Annee = model.Annee;
    groupToUpdate.Subject = model.Subject;
    groupToUpdate.IsActive = model.IsActive;
    groupToUpdate.StudentIds = model.SelectedStudentIds ?? new List<string>();

    await _groupService.UpdateAsync(groupToUpdate);

    return RedirectToAction("AllGroups", "Admin");
}


        // ✅ Supprimer un groupe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            await _groupService.DeleteAsync(id);

            return RedirectToAction("AllGroups", "Admin");
        }
    }
}
