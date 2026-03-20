import { Component, signal } from '@angular/core';
import { NgIf } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [NgIf, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  protected readonly userName = signal('');
  protected readonly password = signal('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  protected onUserNameInput(event: Event): void {
    this.userName.set((event.target as HTMLInputElement).value);
  }

  protected onPasswordInput(event: Event): void {
    this.password.set((event.target as HTMLInputElement).value);
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();
    this.error.set(null);

    const userName = this.userName().trim();
    const password = this.password();
    if (!userName || !password) {
      this.error.set('Username and password are required.');
      return;
    }

    this.loading.set(true);
    this.authService.login(userName, password).subscribe({
      next: () => {
        this.router.navigateByUrl('/products');
      },
      error: (error: unknown) => {
        const message = error instanceof Error ? error.message : 'Could not login. Please try again.';
        this.error.set(message);
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }
}
