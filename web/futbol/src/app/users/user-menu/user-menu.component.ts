import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-user-menu',
  templateUrl: './user-menu.component.html',
  styleUrls: ['./user-menu.component.scss']
})
export class UserMenuComponent implements OnInit {

  isLogged = false;
  version: string | undefined;
  constructor() { }

  ngOnInit(): void {
    this.version = environment.version;
  }

  userLogin() {
    debugger;
  }

}
