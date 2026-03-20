import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';

/** Minimal routed view so `provideRouter` is valid without loading every feature module. */
@Component({ standalone: true, template: '<p>test</p>' })
class TestShellComponent {}

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([
          { path: '', pathMatch: 'full', component: TestShellComponent },
          { path: 'products', component: TestShellComponent },
          { path: '**', redirectTo: '' }
        ])
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    // Arrange: TestBed configured in beforeEach()
    // Act
    const fixture = TestBed.createComponent(App);
    // Assert
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render a router outlet', () => {
    // Arrange
    const fixture = TestBed.createComponent(App);
    // Act
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    // Assert
    expect(compiled.querySelector('router-outlet')).toBeTruthy();
  });

  it('should show Home link when not on home route', async () => {
    // Arrange
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);
    // Act
    await router.navigateByUrl('/products');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const homeLink = compiled.querySelector('.top-home a[routerLink="/"]');
    // Assert
    expect(homeLink?.textContent?.trim()).toBe('Home');
  });
});
