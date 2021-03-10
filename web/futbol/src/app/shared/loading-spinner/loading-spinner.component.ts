import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: '<mat-spinner></mat-spinner>',
  styleUrls: ['./loading-spinner.component.scss']
})
export class LoadingSpinnerComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
