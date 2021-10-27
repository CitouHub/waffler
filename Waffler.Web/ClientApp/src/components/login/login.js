import React, { useState } from 'react';
import { useHistory } from "react-router-dom";
import { Button, FormGroup, Input } from "reactstrap";
import { Form, Field } from "react-final-form";

import Cache from '../../util/cache'
import ProfileService from '../../services/profile.service'

import waffle from '../../assets/images/waffle.png';
import './login.css';

const Login = (props) => {
    const [passwordChecked, setPasswordChecked] = useState(false);
    const [validPassword, setPasswordValid] = useState(false);
    let history = useHistory();

    const onSubmit = values => {
        ProfileService.verifyPassword(values.password).then((success) => {
            setPasswordChecked(true);
            if (success === true) {
                setPasswordValid(true);
                Cache.set("isAuthenticated", true);
                history.push("/");
            } else {
                setPasswordValid(false);
            }
        });
    };

    return (
        <div className="profile">
            <img src={waffle} alt="Logo" className="waffle" />
            <h1>welcome back!</h1>
            <Form
                onSubmit={onSubmit}
                validate={values => {
                    const errors = {};
                    if (!values.password) {
                        errors.password = "Required";
                    }
                    return errors;
                }}
                render={({ handleSubmit, values, submitting, validating, valid }) => (
                    <form onSubmit={handleSubmit}>
                        <FormGroup>
                            <Field name="password">
                                {({ input, meta }) => (
                                    <div>
                                        <Input
                                            {...input}
                                            type="password"
                                            placeholder="password"
                                            invalid={meta.error && meta.touched}
                                        />
                                        {meta.error && meta.touched && <span>{meta.error}</span>}
                                        {!validPassword && passwordChecked && <span>invalid password</span>}
                                    </div>
                                )}
                            </Field>
                        </FormGroup>
                        <Button type="submit" color="info" disabled={!valid}>
                            Get waffling!
                        </Button>
                    </form>
                )}
            />
        </div>
    )
};

export default Login;
