import { Component } from '@angular/core';
import { NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NgIf, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(protected readonly auth: AuthService) {}

  protected logout(): void {
    this.auth.logout();
    // Re-load the home page so auth-derived values refresh.
    window.location.href = '/';
  }
}

