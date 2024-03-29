﻿import React, { useState, useEffect } from 'react';
import { Redirect, useHistory } from "react-router-dom";
import { Button, FormGroup, Input } from 'reactstrap';
import { Form, Field } from 'react-final-form';

import Cache from '../../util/cache';
import ProfileService from '../../services/profile.service'

import waffle from '../../assets/images/waffle.png';
import './login.css';

const NewProfile = () => {
    let active = true;
    const [loading, setLoading] = useState(true);
    const [hasProfile, setHasProfile] = useState();

    useEffect(() => {
        ProfileService.hasProfile().then((value) => {
            if (active) {
                setHasProfile(value);
                setLoading(false);
            }
        });

        return () => {
            active = false;
        }
    }, []);

    let history = useHistory();

    const onSubmit = (values) => {
        ProfileService.createProfile(values.password).then((success) => {
            history.push("/");
        });
    };

    const isAuthenticated = Cache.getAndReset("isAuthenticated");
    if (isAuthenticated) {
        return <Redirect to="/" />
    } else {
        if (!loading && !hasProfile) {
            return (
                <div className="p-center profile">
                    <img src={waffle} alt="Logo" className="waffle" />
                    <h1>welcome new waffler!</h1>
                    <Form
                        onSubmit={onSubmit}
                        validate={values => {
                            const errors = {};
                            if (!values.password) {
                                errors.password = "required";
                            } else if (values.password.length <= 8) {
                                errors.password = "too short";
                            }
                            if (!values.confirmPassword) {
                                errors.confirmPassword = "required";
                            } else if (values.confirmPassword !== values.password) {
                                errors.confirmPassword = "password does not match";
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
                                                    autoFocus
                                                    placeholder="password"
                                                    invalid={meta.error && meta.touched}
                                                />
                                                {meta.error && meta.touched && <span>{meta.error}</span>}
                                            </div>
                                        )}
                                    </Field>
                                </FormGroup>
                                <FormGroup>
                                    <Field name="confirmPassword">
                                        {({ input, meta }) => (
                                            <div>
                                                <Input
                                                    {...input}
                                                    type="password"
                                                    placeholder="confirm password"
                                                    invalid={meta.error && meta.touched}
                                                />
                                                {meta.error && meta.touched && <span>{meta.error}</span>}
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
        } else if (hasProfile) {
            return <Redirect to="/login" />
        } else {
            return null;
        }
    }
};

export default NewProfile;
