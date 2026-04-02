import { Component, signal, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GroupService } from '../service/group.service';
import { QuizService } from '../service/quiz.service';
import { UserService } from '../service/user.service';
import { AuthService } from '../service/auth.service';
import { GroupDto, QuizDto, UserDto } from '../models/models';

@Component({
  selector: 'app-group-manage',
  standalone: true,
  imports: [RouterLink, CommonModule, DatePipe, FormsModule],
  templateUrl: './group-manage.component.html',
  styleUrl: './group-manage.component.css',
})
export class GroupManageComponent implements OnInit {
  private groupSvc = inject(GroupService);
  private quizSvc = inject(QuizService);
  private userSvc = inject(UserService);
  private auth = inject(AuthService);

  groups = signal<GroupDto[]>([]);
  myQuizzes = signal<QuizDto[]>([]);
  loading = signal(true);
  error = signal('');
  success = signal('');

  // Create group form
  newGroupName = '';
  newGroupDesc = '';
  creating = signal(false);

  // Selected group for member management
  selectedGroup = signal<GroupDto | null>(null);

  // User search dropdown
  userSearch = '';
  userResults = signal<UserDto[]>([]);
  showUserDropdown = signal(false);
  addingMember = signal(false);
  memberError = signal('');

  ngOnInit() {
    this.loadGroups();
    const uid = this.auth.currentUser()?.userId;
    this.quizSvc.getAll().subscribe({
      next: q => this.myQuizzes.set(q.filter(x => x.creatorId === uid)),
      error: () => {}
    });
  }

  loadGroups() {
    this.loading.set(true);
    this.groupSvc.getMyGroups().subscribe({
      next: g => { this.groups.set(g); this.loading.set(false); },
      error: () => { this.error.set('Failed to load groups.'); this.loading.set(false); }
    });
  }

  createGroup() {
    if (!this.newGroupName.trim()) return;
    this.creating.set(true);
    this.error.set('');
    this.groupSvc.create({ groupName: this.newGroupName.trim(), description: this.newGroupDesc.trim() || undefined }).subscribe({
      next: g => {
        this.groups.update(list => [...list, g]);
        this.newGroupName = '';
        this.newGroupDesc = '';
        this.creating.set(false);
        this.success.set('Group created!');
        setTimeout(() => this.success.set(''), 3000);
      },
      error: (e) => { this.creating.set(false); this.error.set(e?.error?.message || 'Failed to create group.'); }
    });
  }

  deleteGroup(groupId: string) {
    if (!confirm('Delete this group? Quizzes assigned to it will become open.')) return;
    this.groupSvc.delete(groupId).subscribe({
      next: () => {
        this.groups.update(list => list.filter(g => g.groupId !== groupId));
        if (this.selectedGroup()?.groupId === groupId) this.selectedGroup.set(null);
      },
      error: () => this.error.set('Failed to delete group.')
    });
  }

  selectGroup(g: GroupDto) {
    this.selectedGroup.set(g);
    this.userSearch = '';
    this.userResults.set([]);
    this.showUserDropdown.set(false);
    this.memberError.set('');
  }

  onUserSearchInput() {
    const q = this.userSearch.trim();
    if (q.length < 1) { this.userResults.set([]); this.showUserDropdown.set(false); return; }
    this.userSvc.getTakers(q).subscribe({
      next: users => {
        const existing = new Set(this.selectedGroup()?.members.map(m => m.userId) ?? []);
        const filtered = users.filter(u => !existing.has(u.userId));
        this.userResults.set(filtered);
        this.showUserDropdown.set(true);
      },
      error: () => { this.showUserDropdown.set(false); }
    });
  }

  selectUser(user: UserDto) {
    const g = this.selectedGroup();
    if (!g) return;
    this.addingMember.set(true);
    this.memberError.set('');
    this.showUserDropdown.set(false);
    this.userSearch = '';
    this.userResults.set([]);
    this.groupSvc.addMember(g.groupId, user.username).subscribe({
      next: (member) => {
        this.selectedGroup.update(sg => sg ? { ...sg, members: [...sg.members, member], memberCount: sg.memberCount + 1 } : sg);
        this.groups.update(list => list.map(x => x.groupId === g.groupId ? { ...x, memberCount: x.memberCount + 1 } : x));
        this.addingMember.set(false);
      },
      error: (e) => { this.addingMember.set(false); this.memberError.set(e?.error?.detail || e?.error?.message || 'Failed to add user.'); }
    });
  }

  addMember() {
    // fallback: add by typed username/email if no dropdown selection
    const g = this.selectedGroup();
    if (!g || !this.userSearch.trim()) return;
    this.addingMember.set(true);
    this.memberError.set('');
    this.showUserDropdown.set(false);
    this.groupSvc.addMember(g.groupId, this.userSearch.trim()).subscribe({
      next: (member) => {
        this.selectedGroup.update(sg => sg ? { ...sg, members: [...sg.members, member], memberCount: sg.memberCount + 1 } : sg);
        this.groups.update(list => list.map(x => x.groupId === g.groupId ? { ...x, memberCount: x.memberCount + 1 } : x));
        this.userSearch = '';
        this.addingMember.set(false);
      },
      error: (e) => { this.addingMember.set(false); this.memberError.set(e?.error?.detail || e?.error?.message || 'User not found.'); }
    });
  }

  removeMember(userId: string) {
    const g = this.selectedGroup();
    if (!g) return;
    this.groupSvc.removeMember(g.groupId, userId).subscribe({
      next: () => {
        this.selectedGroup.update(sg => sg ? { ...sg, members: sg.members.filter(m => m.userId !== userId), memberCount: sg.memberCount - 1 } : sg);
        this.groups.update(list => list.map(x => x.groupId === g.groupId ? { ...x, memberCount: x.memberCount - 1 } : x));
      },
      error: () => this.memberError.set('Failed to remove member.')
    });
  }

  assignQuiz(quizId: string, groupId: string) {
    this.groupSvc.assignQuiz(groupId, quizId).subscribe({
      next: () => {
        this.myQuizzes.update(list => list.map(q => q.quizId === quizId ? { ...q, groupId } : q));
        this.success.set('Quiz assigned to group!');
        setTimeout(() => this.success.set(''), 3000);
      },
      error: () => this.error.set('Failed to assign quiz.')
    });
  }

  unassignQuiz(quizId: string) {
    this.groupSvc.unassignQuiz(quizId).subscribe({
      next: () => {
        this.myQuizzes.update(list => list.map(q => q.quizId === quizId ? { ...q, groupId: undefined } : q));
        this.success.set('Quiz is now open to all.');
        setTimeout(() => this.success.set(''), 3000);
      },
      error: () => this.error.set('Failed to unassign quiz.')
    });
  }

  getGroupName(groupId: string): string {
    return this.groups().find(g => g.groupId === groupId)?.groupName ?? '';
  }
}
