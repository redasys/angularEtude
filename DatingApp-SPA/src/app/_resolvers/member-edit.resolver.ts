import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { catchError } from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MemberEditResolver implements Resolve<User> {

    constructor(private userService: UserService, private router: Router,
        private alertify: AlertifyService,
        private authService: AuthService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<User> {
        console.log(this.authService.decodedToken);
        return this.userService.getUser(this.authService.decodedToken.nameid).pipe(
            catchError(error => {
                this.alertify.error('Couldn\'t retrieve your data.');
                this.router.navigate(['/members']);
                return of(null);
            })
        );
    }
}
