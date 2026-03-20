import { registerLocaleData } from '@angular/common';
import localeGb from '@angular/common/locales/en-GB';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

registerLocaleData(localeGb);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
