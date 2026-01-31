import { Component } from '@angular/core';
import { RegisterComponent } from './features/auth/register/register.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RegisterComponent],
  template: '<app-register></app-register>',
  styles: []
})
export class AppComponent {}