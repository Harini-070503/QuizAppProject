import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CategoryService } from './category.service';
import { CategoryDto, CategoryCreateDto } from '../models/models';

describe('CategoryService', () => {
  let service: CategoryService;
  let httpMock: HttpTestingController;
  const BASE = 'http://localhost:5137/api/categories';

  const mockCategory: CategoryDto = { categoryId: 'cat-1', categoryName: 'Science' };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CategoryService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(CategoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => expect(service).toBeTruthy());

  describe('getAll()', () => {
    it('should GET all categories', () => {
      service.getAll().subscribe(cats => {
        expect(cats.length).toBe(1);
        expect(cats[0].categoryName).toBe('Science');
      });
      const req = httpMock.expectOne(BASE);
      expect(req.request.method).toBe('GET');
      req.flush([mockCategory]);
    });

    it('should return empty array when no categories', () => {
      service.getAll().subscribe(cats => expect(cats).toEqual([]));
      httpMock.expectOne(BASE).flush([]);
    });
  });

  describe('get()', () => {
    it('should GET a category by id', () => {
      service.get('cat-1').subscribe(c => expect(c.categoryName).toBe('Science'));
      const req = httpMock.expectOne(`${BASE}/cat-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCategory);
    });
  });

  describe('create()', () => {
    it('should POST a new category', () => {
      const dto: CategoryCreateDto = { categoryName: 'History' };
      service.create(dto).subscribe(c => expect(c.categoryId).toBe('cat-1'));
      const req = httpMock.expectOne(BASE);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      req.flush(mockCategory);
    });
  });

  describe('update()', () => {
    it('should PUT to update a category', () => {
      const dto: CategoryCreateDto = { categoryName: 'Updated Science' };
      service.update('cat-1', dto).subscribe(c => expect(c).toBeTruthy());
      const req = httpMock.expectOne(`${BASE}/cat-1`);
      expect(req.request.method).toBe('PUT');
      req.flush({ ...mockCategory, categoryName: 'Updated Science' });
    });
  });

  describe('delete()', () => {
    it('should DELETE a category by id', () => {
      service.delete('cat-1').subscribe();
      const req = httpMock.expectOne(`${BASE}/cat-1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should propagate error when delete fails', () => {
      let errorOccurred = false;
      service.delete('cat-1').subscribe({ error: () => errorOccurred = true });
      httpMock.expectOne(`${BASE}/cat-1`).flush(
        { message: 'Has related quizzes' }, { status: 400, statusText: 'Bad Request' }
      );
      expect(errorOccurred).toBeTrue();
    });
  });
});
