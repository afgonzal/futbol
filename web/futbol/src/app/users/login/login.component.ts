import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SocialAuthService, GoogleLoginProvider } from 'angularx-social-login';
import { SpfAuthService } from 'src/app/auth/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  isLoading: boolean = false;
  constructor( private authService: SocialAuthService, private userService: SpfAuthService,
    private activated: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void {
  }

  googleLogin() : void {
    this.isLoading = true;
    this.authService.signIn(GoogleLoginProvider.PROVIDER_ID).then((response) => {
      debugger;
      this.userService.login({
        userId: response.id,
        firstName: response.firstName,
        lastName: response.lastName,
        email: response.email,
        pictureUrl: response.photoUrl,
        idToken: response.idToken
      });
    }).finally(() => {
      this.isLoading = false;
    });
  }

}
