import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { StartPageComponent } from './start-page.component';

describe('StartPageComponent', () => {
  let component: StartPageComponent;
  let fixture: ComponentFixture<StartPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StartPageComponent],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(StartPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('should generate 20 particles on init', () => {
    expect(component.particles.length).toBe(20);
  });

  it('should give each particle a random x between 0 and 100', () => {
    component.particles.forEach(p => {
      expect(p.x).toBeGreaterThanOrEqual(0);
      expect(p.x).toBeLessThanOrEqual(100);
    });
  });

  it('should give each particle an animation delay between 0 and 10', () => {
    component.particles.forEach(p => {
      expect(p.delay).toBeGreaterThanOrEqual(0);
      expect(p.delay).toBeLessThanOrEqual(10);
    });
  });

  it('should give each particle a duration between 5 and 15', () => {
    component.particles.forEach(p => {
      expect(p.dur).toBeGreaterThanOrEqual(5);
      expect(p.dur).toBeLessThanOrEqual(15);
    });
  });

  describe('template', () => {
    it('should render the brand headline text', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Challenge Your');
      expect(el.textContent).toContain('Knowledge');
    });

    it('should render a "Start Playing" link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Start Playing');
    });

    it('should render a "Create Account" link', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Create Account');
    });

    it('should render stat numbers', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('500+');
      expect(el.textContent).toContain('10K+');
      expect(el.textContent).toContain('50+');
    });

    it('should render the quiz preview card', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.quiz-card-preview')).not.toBeNull();
    });

    it('should render the score card float', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.querySelector('.score-card-float')).not.toBeNull();
    });

    it('should render particle divs', () => {
      const el = fixture.nativeElement as HTMLElement;
      const particles = el.querySelectorAll('.particle');
      expect(particles.length).toBe(20);
    });

    it('should render the "Live Quiz Platform" badge', () => {
      const el = fixture.nativeElement as HTMLElement;
      expect(el.textContent).toContain('Live Quiz Platform');
    });
  });
});
