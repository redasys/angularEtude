import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { catchError } from 'rxjs/operators';
import { of, Observable } from 'rxjs';

@Injectable()
export class ListResolver implements Resolve<User[]> {
    pageNumber = 1;
    pageSize = 6;
    likesParam = 'Likers';

    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        return this.userService.getUsers(this.pageNumber, this.pageSize, null, this.likesParam).pipe(
            catchError(error => {
                this.alertify.error('Couldn\'t retrieve the list.');
                this.router.navigate(['']);
                return of(null);
            })
        );
    }
}
