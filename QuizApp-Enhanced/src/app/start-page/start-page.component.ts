import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';

@Component({
  templateUrl: './start-page.component.html',
  styleUrl: './start-page.component.css',
  selector: 'app-start-page',
  standalone: true,
  imports: [RouterLink],

})
export class StartPageComponent {
  particles = Array.from({ length: 20 }, (_, i) => ({
    x: Math.random() * 100,
    delay: Math.random() * 10,
    dur: 5 + Math.random() * 10
  }));
}
