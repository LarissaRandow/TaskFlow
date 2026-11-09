import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

type BrandVariant = 'authentication' | 'dashboard';

@Component({
  imports: [RouterLink],
  selector: 'app-brand-header',
  styleUrl: './brand-header.scss',
  templateUrl: './brand-header.html',
  host: {
    '[class.dashboard-brand]': "variant() === 'dashboard'",
  },
})
export class BrandHeader {
  readonly ariaLabel = input('TaskFlow');
  readonly link = input('/dashboard');
  readonly tagline = input(false);
  readonly variant = input<BrandVariant>('authentication');
}
