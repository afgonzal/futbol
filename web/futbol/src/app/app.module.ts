import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { SocialLoginModule,  GoogleLoginProvider, SocialAuthServiceConfig } from 'angularx-social-login';
import { environment } from 'src/environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule } from '@angular/common/http';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppComponent } from './app.component';
import { LoginComponent } from './users/login/login.component';
import { HeaderComponent } from './shared/header/header.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { UserMenuComponent } from './users/user-menu/user-menu.component';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { LayoutModule } from '@angular/cdk/layout';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';


import { HomeComponent } from './home/home.component';
import { DashboardComponent } from './shared/dashboard/dashboard.component';
import { MatGridListModule } from '@angular/material/grid-list';
import { LoadingSpinnerComponent } from './shared/loading-spinner/loading-spinner.component';
import { SpfAuthService } from './auth/auth.service';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    HeaderComponent,
    UserMenuComponent,
    HomeComponent,
    DashboardComponent,
    LoadingSpinnerComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    SocialLoginModule,
    BrowserAnimationsModule,
    MatButtonModule,
    MatToolbarModule,
    MatMenuModule,
    MatIconModule,
    LayoutModule,
    MatDividerModule,
    MatCardModule,
    MatGridListModule,
    MatProgressSpinnerModule,
    MatProgressBarModule
  ],
  providers: [
    {
      provide: 'SocialAuthServiceConfig',
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(environment.GoogleClientId, { scope: 'profile email'})
          }
        ]
      } as SocialAuthServiceConfig,
    },
    SpfAuthService

  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
