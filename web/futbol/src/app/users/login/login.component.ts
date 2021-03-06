import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, GoogleLoginProvider } from 'angularx-social-login';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  constructor( private authService: AuthService,
    private activated: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void {
  }

  logInWithGoogle() {
    this.authService.signIn(GoogleLoginProvider.PROVIDER_ID);
  }

}