import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { CategoryManageComponent } from './category-manage.component';
import { CategoryService } from '../service/category.service';
import { AuthService } from '../service/auth.service';
import { CategoryDto } from '../models/models';

describe('CategoryManageComponent', () => {
  let component: CategoryManageComponent;
  let fixture: ComponentFixture<CategoryManageComponent>;
  let catSvc: jasmine.SpyObj<CategoryService>;

  const mockCategories: CategoryDto[] = [
    { categoryId: 'c1', categoryName: 'Science' },
    { categoryId: 'c2', categoryName: 'History' }
  ];

  beforeEach(async () => {
    const catSpy  = jasmine.createSpyObj('CategoryService', ['getAll', 'create', 'update', 'delete']);
    const authSpy = jasmine.createSpyObj('AuthService', ['currentUser', 'isLoggedIn', 'token', 'isCreator']);

    catSpy.getAll.and.returnValue(of(mockCategories));
    catSpy.create.and.returnValue(of({ categoryId: 'c3', categoryName: 'Math' }));
    catSpy.update.and.returnValue(of({ categoryId: 'c1', categoryName: 'Updated Science' }));
    catSpy.delete.and.returnValue(of(void 0));
    authSpy.currentUser.and.returnValue({ userId: 'u1', username: 'creator', email: 'c@c.com', role: 'Creator' });
    authSpy.isLoggedIn.and.returnValue(true);
    authSpy.isCreator.and.returnValue(true);
    authSpy.token.and.returnValue('tok');

    await TestBed.configureTestingModule({
      imports: [CategoryManageComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: CategoryService, useValue: catSpy },
        { provide: AuthService,     useValue: authSpy }
      ]
    }).compileComponents();

    catSvc  = TestBed.inject(CategoryService) as jasmine.SpyObj<CategoryService>;
    fixture = TestBed.createComponent(CategoryManageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should load categories on init', () => {
    expect(catSvc.getAll).toHaveBeenCalled();
    expect(component.categories().length).toBe(2);
  });

  it('should set loading to false after load', () => {
    expect(component.loading()).toBeFalse();
  });

  it('should initialise signals correctly', () => {
    expect(component.saving()).toBeFalse();
    expect(component.error()).toBe('');
    expect(component.success()).toBe('');
    expect(component.catName).toBe('');
    expect(component.editId()).toBe('');
  });

  describe('onCreate()', () => {
    it('should call categoryService.create with catName', () => {
      component.catName = 'Math';
      component.onCreate();
      expect(catSvc.create).toHaveBeenCalledWith({ categoryName: 'Math' });
    });

    it('should set error when catName is empty', () => {
      component.catName = '';
      component.onCreate();
      expect(component.error()).toBe('Category name is required.');
      expect(catSvc.create).not.toHaveBeenCalled();
    });

    it('should clear catName after successful create', () => {
      component.catName = 'Math';
      component.onCreate();
      expect(component.catName).toBe('');
    });

    it('should set success message after create', () => {
      component.catName = 'Math';
      component.onCreate();
      expect(component.success()).toBe('Category created!');
    });

    it('should reload categories after create', () => {
      component.catName = 'Math';
      component.onCreate();
      // getAll called once on init + once after create
      expect(catSvc.getAll).toHaveBeenCalledTimes(2);
    });

    it('should set error on create failure', () => {
      catSvc.create.and.returnValue(throwError(() => ({ error: { message: 'Already exists.' } })));
      component.catName = 'Science';
      component.onCreate();
      expect(component.error()).toBe('Already exists.');
    });
  });

  describe('startEdit() / cancelEdit()', () => {
    it('should set editId and catName from selected category', () => {
      component.startEdit(mockCategories[0]);
      expect(component.editId()).toBe('c1');
      expect(component.catName).toBe('Science');
    });

    it('should clear editId and catName on cancelEdit', () => {
      component.startEdit(mockCategories[0]);
      component.cancelEdit();
      expect(component.editId()).toBe('');
      expect(component.catName).toBe('');
    });
  });

  describe('onUpdate()', () => {
    it('should call categoryService.update with editId and new name', () => {
      component.startEdit(mockCategories[0]);
      component.catName = 'Updated Science';
      component.onUpdate();
      expect(catSvc.update).toHaveBeenCalledWith('c1', { categoryName: 'Updated Science' });
    });

    it('should set error if name is empty during update', () => {
      component.startEdit(mockCategories[0]);
      component.catName = '';
      component.onUpdate();
      expect(component.error()).toBe('Name required.');
      expect(catSvc.update).not.toHaveBeenCalled();
    });

    it('should cancel edit after successful update', () => {
      component.startEdit(mockCategories[0]);
      component.catName = 'Updated';
      component.onUpdate();
      expect(component.editId()).toBe('');
    });
  });

  describe('onDelete()', () => {
    it('should call categoryService.delete after confirm', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      component.onDelete('c1');
      expect(catSvc.delete).toHaveBeenCalledWith('c1');
    });

    it('should NOT call delete if user cancels confirm', () => {
      spyOn(window, 'confirm').and.returnValue(false);
      component.onDelete('c1');
      expect(catSvc.delete).not.toHaveBeenCalled();
    });

    it('should reload categories after delete', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      component.onDelete('c1');
      expect(catSvc.getAll).toHaveBeenCalledTimes(2);
    });

    it('should set error message on delete failure', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      catSvc.delete.and.returnValue(throwError(() => new Error('FK constraint')));
      component.onDelete('c1');
      expect(component.error()).toBe('Failed to delete. It may have quizzes linked.');
    });
  });
});
