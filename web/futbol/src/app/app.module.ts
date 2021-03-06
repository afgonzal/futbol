import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { SocialLoginModule, AuthServiceConfig, GoogleLoginProvider } from 'angularx-social-login';
import { environment } from 'src/environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './users/login/login.component';

const config = new AuthServiceConfig([
  {
     id: GoogleLoginProvider.PROVIDER_ID,
     provider: new GoogleLoginProvider(environment.GoogleClientId, {
      scope: 'profile email'
    })
  },
]);
export function provideConfig() {
    return config;
}

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    SocialLoginModule
  ],
  providers: [ {provide: AuthServiceConfig, useFactory: provideConfig}],
  bootstrap: [AppComponent]
})
export class AppModule { }
