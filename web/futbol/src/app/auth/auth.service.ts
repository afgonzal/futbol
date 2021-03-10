import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs";
import { environment } from "src/environments/environment";
import { IUserModel } from "../users/user.model";

@Injectable()

export class SpfAuthService implements OnDestroy {
  userData: IUserModel | undefined;
  apiIdentifyValidateSubscription: Subscription | undefined;
  constructor(private http: HttpClient) {

  }

  login(userData: IUserModel) : Promise<boolean> {
    return new Promise( resolve =>  {
      if (!userData || !userData.idToken)
      {
        resolve(false);
      }
      else {
        this.userData = userData;

        this.apiIdentifyValidateSubscription = this.http.get(environment.API_URL + 'identity/validate', {
          params: new HttpParams().set('token', userData.idToken)
        }).subscribe( (response : any) => {
          debugger;
          if (response.isAuthorized) {
            resolve(true);
          } else {
            resolve(false);
          }
        });
      }
    });
  }


  ngOnDestroy(): void {
    this.apiIdentifyValidateSubscription?.unsubscribe();
  }

}
